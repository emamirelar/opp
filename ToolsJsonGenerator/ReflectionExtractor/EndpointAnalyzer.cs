using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using ReflectionExtractor.Models;

namespace ReflectionExtractor;

public class EndpointAnalyzer
{
    public async Task<string> ExtractEndpointsAsync(string dllPath, string xmlPath)
    {
        Console.WriteLine("[INFO] Loading assembly and XML documentation...");
        
        // Load the assembly
        var assembly = LoadAssembly(dllPath);
        
        // Load XML documentation
        var xmlParser = new XmlDocumentationParser(xmlPath);
        
        // Extract endpoint data
        var endpointData = new ExtractedEndpointData
        {
            AssemblyName = assembly.GetName().Name ?? "Unknown",
            AssemblyVersion = assembly.GetName().Version?.ToString() ?? "Unknown",
            ExtractedAt = DateTime.UtcNow
        };

        // Extract search metadata FIRST
        Console.WriteLine("[INFO] Extracting search metadata...");
        var searchExtractor = new SearchMetadataExtractor();
        endpointData.SearchMetadata = searchExtractor.ExtractSearchMetadata(assembly);

        // Find all controller types
        var controllerTypes = FindControllerTypes(assembly);
        Console.WriteLine($"[FOUND] {controllerTypes.Count} controller types");

        foreach (var controllerType in controllerTypes)
        {
            var controllerInfo = AnalyzeController(controllerType, xmlParser);
            
            // Attach search metadata to relevant controllers
            AttachSearchMetadataToController(controllerInfo, endpointData.SearchMetadata);
            
            if (controllerInfo.Methods.Any())
            {
                endpointData.Controllers.Add(controllerInfo);
                Console.WriteLine($"   [OK] {controllerInfo.Name}: {controllerInfo.Methods.Count} endpoints");
            }
        }

        // Serialize to JSON
        var json = JsonConvert.SerializeObject(endpointData, Formatting.Indented);
        var totalEndpoints = endpointData.Controllers.Sum(c => c.Methods.Count);
        Console.WriteLine($"[SUCCESS] Extracted {totalEndpoints} endpoints from {endpointData.Controllers.Count} controllers");
        Console.WriteLine($"[SUCCESS] Extracted search metadata for {endpointData.SearchMetadata.Count} entities");
        
        return json;
    }

    private Assembly LoadAssembly(string dllPath)
    {
        try
        {
            // Enable assembly resolution from the same directory
            var directory = Path.GetDirectoryName(dllPath);
            if (!string.IsNullOrEmpty(directory))
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var assemblyName = new AssemblyName(args.Name).Name;
                    var assemblyPath = Path.Combine(directory, assemblyName + ".dll");
                    return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
                };
            }

            return Assembly.LoadFrom(dllPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load assembly from {dllPath}: {ex.Message}", ex);
        }
    }

    private List<Type> FindControllerTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.Name.EndsWith("Controller") || 
                          type.IsSubclassOf(typeof(ControllerBase)) ||
                          type.GetCustomAttribute<ApiControllerAttribute>() != null)
            .Where(type => !IsExternalController(type)) // Exclude external controllers
            .ToList();
    }

    private bool IsExternalController(Type controllerType)
    {
        // Exclude controllers from External namespaces or with External in the name
        var fullName = controllerType.FullName ?? controllerType.Name;
        var namespaceName = controllerType.Namespace ?? "";
        
        bool isExternal = fullName.Contains(".External.") || 
                         namespaceName.Contains(".External") ||
                         controllerType.Name.Contains("External");
        
        if (isExternal)
        {
            Console.WriteLine($"[EXCLUDED] External controller: {fullName}");
        }
        
        return isExternal;
    }

    private ControllerInfo AnalyzeController(Type controllerType, XmlDocumentationParser xmlParser)
    {
        var controllerInfo = new ControllerInfo
        {
            Name = controllerType.Name,
            FullName = controllerType.FullName ?? controllerType.Name,
            BaseRoute = ExtractBaseRoute(controllerType),
            Attributes = GetAttributeStrings(controllerType.GetCustomAttributes())
        };

        // Find all action methods
        var methods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => IsActionMethod(method))
            .ToList();

        foreach (var method in methods)
        {
            var methodInfo = AnalyzeMethod(method, controllerType, controllerInfo.BaseRoute, xmlParser);
            if (methodInfo != null)
            {
                controllerInfo.Methods.Add(methodInfo);
            }
        }

        return controllerInfo;
    }

    private string ExtractBaseRoute(Type controllerType)
    {
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        if (routeAttr != null)
        {
            return routeAttr.Template ?? string.Empty;
        }

        // Default convention-based routing
        var controllerName = controllerType.Name;
        if (controllerName.EndsWith("Controller"))
        {
            controllerName = controllerName[..^10]; // Remove "Controller"
        }
        
        return $"api/{controllerName.ToLower()}";
    }

    private bool IsActionMethod(System.Reflection.MethodInfo method)
    {
        // Skip inherited methods, constructors, and non-public methods
        if (method.DeclaringType != method.ReflectedType) return false;
        if (method.IsSpecialName) return false;
        
        // Check for HTTP method attributes
        var httpAttributes = new[]
        {
            typeof(HttpGetAttribute),
            typeof(HttpPostAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpDeleteAttribute),
            typeof(HttpPatchAttribute),
            typeof(HttpOptionsAttribute),
            typeof(HttpHeadAttribute)
        };

        return httpAttributes.Any(attrType => method.GetCustomAttribute(attrType) != null);
    }

    private Models.MethodInfo? AnalyzeMethod(System.Reflection.MethodInfo method, Type controllerType, string baseRoute, XmlDocumentationParser xmlParser)
    {
        var httpMethod = GetHttpMethod(method);
        if (string.IsNullOrEmpty(httpMethod)) return null;

        var route = GetMethodRoute(method);
        var fullRoute = CombineRoutes(baseRoute, route);

        // Get XML documentation
        var xmlName = xmlParser.BuildMethodXmlName(controllerType, method);
        var documentation = xmlParser.GetMethodDocumentation(xmlName);

        var methodInfo = new Models.MethodInfo
        {
            Name = method.Name,
            HttpMethod = httpMethod,
            Route = route,
            FullRoute = fullRoute,
            Summary = documentation?.Summary ?? string.Empty,
            Returns = documentation?.Returns ?? string.Empty,
            ExampleUses = documentation?.ExampleUses ?? new List<string>(),
            WhenToUse = documentation?.WhenToUse ?? string.Empty,
            Attributes = GetAttributeStrings(method.GetCustomAttributes()),
            AccessControl = GetAccessControlInfo(method),
            Parameters = AnalyzeParameters(method, documentation)
        };

        return methodInfo;
    }

    private string GetHttpMethod(System.Reflection.MethodInfo method)
    {
        if (method.GetCustomAttribute<HttpGetAttribute>() != null) return "GET";
        if (method.GetCustomAttribute<HttpPostAttribute>() != null) return "POST";
        if (method.GetCustomAttribute<HttpPutAttribute>() != null) return "PUT";
        if (method.GetCustomAttribute<HttpDeleteAttribute>() != null) return "DELETE";
        if (method.GetCustomAttribute<HttpPatchAttribute>() != null) return "PATCH";
        if (method.GetCustomAttribute<HttpOptionsAttribute>() != null) return "OPTIONS";
        if (method.GetCustomAttribute<HttpHeadAttribute>() != null) return "HEAD";
        
        return string.Empty;
    }

    private string GetMethodRoute(System.Reflection.MethodInfo method)
    {
        // Check HTTP method attributes for route templates
        var httpGetAttr = method.GetCustomAttribute<HttpGetAttribute>();
        if (httpGetAttr != null) return httpGetAttr.Template ?? string.Empty;

        var httpPostAttr = method.GetCustomAttribute<HttpPostAttribute>();
        if (httpPostAttr != null) return httpPostAttr.Template ?? string.Empty;

        var httpPutAttr = method.GetCustomAttribute<HttpPutAttribute>();
        if (httpPutAttr != null) return httpPutAttr.Template ?? string.Empty;

        var httpDeleteAttr = method.GetCustomAttribute<HttpDeleteAttribute>();
        if (httpDeleteAttr != null) return httpDeleteAttr.Template ?? string.Empty;

        // Check for explicit Route attribute
        var routeAttr = method.GetCustomAttribute<RouteAttribute>();
        if (routeAttr != null) return routeAttr.Template ?? string.Empty;

        return string.Empty;
    }

    private string CombineRoutes(string baseRoute, string methodRoute)
    {
        if (string.IsNullOrEmpty(baseRoute)) return methodRoute;
        if (string.IsNullOrEmpty(methodRoute)) return baseRoute;
        
        var combined = $"{baseRoute.TrimEnd('/')}/{methodRoute.TrimStart('/')}";
        return combined.TrimStart('/');
    }

    private string GetAccessControlInfo(System.Reflection.MethodInfo method)
    {
        var attributes = method.GetCustomAttributes().ToList();
        
        // Look for authorization attributes
        var authAttrs = attributes.Where(attr => 
            attr.GetType().Name.Contains("Authorize") || 
            attr.GetType().Name.Contains("AccessControlled"))
            .ToList();

        if (authAttrs.Any())
        {
            return string.Join(", ", authAttrs.Select(attr => attr.GetType().Name));
        }

        return string.Empty;
    }

    private List<Models.ParameterInfo> AnalyzeParameters(System.Reflection.MethodInfo method, XmlDocumentation? documentation)
    {
        var parameters = new List<Models.ParameterInfo>();
        var methodParams = method.GetParameters();

        foreach (var param in methodParams)
        {
            var paramInfo = new Models.ParameterInfo
            {
                Name = param.Name ?? string.Empty,
                Type = GetTypeDisplayName(param.ParameterType),
                IsRequired = !param.IsOptional && !param.ParameterType.IsNullable(),
                IsFromBody = param.GetCustomAttribute<FromBodyAttribute>() != null,
                IsFromQuery = param.GetCustomAttribute<FromQueryAttribute>() != null,
                IsFromRoute = param.GetCustomAttribute<FromRouteAttribute>() != null,
                DefaultValue = param.DefaultValue,
                Description = documentation?.Parameters.GetValueOrDefault(param.Name ?? string.Empty) ?? string.Empty
            };

            // Extract detailed schema for complex types
            if (IsComplexType(param.ParameterType))
            {
                paramInfo.Schema = AnalyzeModelSchema(param.ParameterType);
                paramInfo.Properties = AnalyzeTypeProperties(param.ParameterType);
            }

            parameters.Add(paramInfo);
        }

        return parameters;
    }

    private bool IsComplexType(Type type)
    {
        // Remove nullable wrapper
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        // Check if it's a primitive, string, DateTime, or common simple types
        if (underlyingType.IsPrimitive || underlyingType == typeof(string) || 
            underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset) ||
            underlyingType == typeof(TimeSpan) || underlyingType == typeof(Guid) ||
            underlyingType == typeof(decimal) || underlyingType.IsEnum)
        {
            return false;
        }

        // Check if it's a collection of simple types
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType) && underlyingType != typeof(string))
        {
            if (underlyingType.IsGenericType)
            {
                var elementType = underlyingType.GetGenericArguments()[0];
                return IsComplexType(elementType);
            }
            return false;
        }

        return true;
    }

    private Models.ModelSchema AnalyzeModelSchema(Type type)
    {
        var schema = new Models.ModelSchema
        {
            TypeName = type.Name,
            FullTypeName = type.FullName ?? type.Name,
            Description = $"Schema for {type.Name}"
        };

        var properties = AnalyzeTypeProperties(type, null, 0);
        schema.Properties = properties;
        schema.RequiredFields = properties.Where(p => p.IsRequired).Select(p => p.Name).ToList();

        return schema;
    }

    private List<Models.PropertyInfo> AnalyzeTypeProperties(Type type, HashSet<Type>? visitedTypes = null, int depth = 0)
    {
        // Prevent infinite recursion with depth limit and visited types tracking
        if (depth > 3 || visitedTypes?.Contains(type) == true)
        {
            return new List<Models.PropertyInfo>();
        }

        visitedTypes ??= new HashSet<Type>();
        visitedTypes.Add(type);

        var properties = new List<Models.PropertyInfo>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propInfo = new Models.PropertyInfo
            {
                Name = prop.Name,
                Type = GetTypeDisplayName(prop.PropertyType),
                IsRequired = IsPropertyRequired(prop),
                IsNullable = IsPropertyNullable(prop),
                Description = GetPropertyDescription(prop),
                Format = GetPropertyFormat(prop),
                Relationship = AnalyzeFieldRelationship(prop)
            };

            // Recursively analyze nested complex types (with depth and cycle protection)
            if (IsComplexType(prop.PropertyType) && !visitedTypes.Contains(prop.PropertyType) && depth < 2)
            {
                propInfo.NestedProperties = AnalyzeTypeProperties(prop.PropertyType, new HashSet<Type>(visitedTypes), depth + 1);
            }

            properties.Add(propInfo);
        }

        return properties;
    }

    private bool IsPropertyRequired(System.Reflection.PropertyInfo prop)
    {
        // Check for Required attribute
        if (prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null)
        {
            return true;
        }

        // Check if it's a non-nullable reference type or value type
        var propertyType = prop.PropertyType;
        if (propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
        {
            return true; // Non-nullable value type
        }

        return false;
    }

    private bool IsPropertyNullable(System.Reflection.PropertyInfo prop)
    {
        var propertyType = prop.PropertyType;
        
        // Check if it's a nullable value type
        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            return true;
        }

        // Check if it's a reference type (could be null)
        if (!propertyType.IsValueType)
        {
            return true;
        }

        return false;
    }

    private string GetPropertyDescription(System.Reflection.PropertyInfo prop)
    {
        // Try to get XML documentation for properties (would need enhancement to XmlDocumentationParser)
        return $"Property of type {GetTypeDisplayName(prop.PropertyType)}";
    }

    private string? GetPropertyFormat(System.Reflection.PropertyInfo prop)
    {
        var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
            return "date-time";
        
        if (propertyType == typeof(DateOnly))
            return "date";
        
        if (propertyType == typeof(TimeOnly) || propertyType == typeof(TimeSpan))
            return "time";

        if (propertyType == typeof(Guid))
            return "uuid";

        // Check for email validation attribute
        if (prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.EmailAddressAttribute>() != null)
            return "email";

        // Check for URL validation attribute  
        if (prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.UrlAttribute>() != null)
            return "uri";

        return null;
    }

    private Models.FieldRelationship? AnalyzeFieldRelationship(System.Reflection.PropertyInfo prop)
    {
        // Check for ForeignKey attribute
        var foreignKeyAttr = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>();
        if (foreignKeyAttr != null)
        {
            return new Models.FieldRelationship
            {
                RelationType = "ForeignKey",
                ReferencedProperty = foreignKeyAttr.Name,
                RequiresIdResolution = true
            };
        }

        // Analyze property name patterns for common ID fields
        var propName = prop.Name.ToLower();
        if (propName.EndsWith("id") && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?)))
        {
            var relationship = new Models.FieldRelationship
            {
                RelationType = "Reference",
                RequiresIdResolution = true
            };

            // Infer referenced entity from property name
            if (propName.Contains("organizationunit") || propName.Contains("orghierarchy"))
            {
                relationship.ReferencedEntity = "OrganizationHierarchy";
                relationship.LookupEndpoint = "api/values/organization-units";
                relationship.DisplayProperty = "Name";
            }
            else if (propName.Contains("partnercategory"))
            {
                relationship.ReferencedEntity = "PartnerCategory";
                relationship.LookupEndpoint = "api/values/partner-categories";
                relationship.DisplayProperty = "Name";
            }
            else if (propName.Contains("liaisonoffice"))
            {
                relationship.ReferencedEntity = "LiaisonOffice";
                relationship.LookupEndpoint = "api/values/liaison-offices";
                relationship.DisplayProperty = "Name";
            }
            else if (propName.Contains("user"))
            {
                relationship.ReferencedEntity = "User";
                relationship.LookupEndpoint = "api/values/users";
                relationship.DisplayProperty = "Name";
            }

            return relationship;
        }

        // Check for string fields that might need lookup (like codes)
        if (prop.PropertyType == typeof(string) && propName.Contains("code"))
        {
            return new Models.FieldRelationship
            {
                RelationType = "Lookup",
                RequiresIdResolution = false
            };
        }

        return null;
    }

    private bool IsCircularReference(Type parentType, Type childType)
    {
        // Simple check to avoid infinite recursion
        return parentType == childType || parentType.FullName == childType.FullName;
    }

    private string GetTypeDisplayName(Type type)
    {
        if (type.IsGenericType)
        {
            var typeName = type.Name.Split('`')[0];
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetTypeDisplayName));
            return $"{typeName}<{genericArgs}>";
        }

        return type.Name switch
        {
            "String" => "string",
            "Int32" => "int",
            "Int64" => "long",
            "Boolean" => "bool",
            "DateTime" => "DateTime",
            "Decimal" => "decimal",
            "Double" => "double",
            "Single" => "float",
            _ => type.Name
        };
    }

    private List<string> GetAttributeStrings(IEnumerable<Attribute> attributes)
    {
        return attributes.Select(attr => attr.GetType().Name).ToList();
    }

    private void AttachSearchMetadataToController(ControllerInfo controllerInfo, List<EntitySearchMetadata> searchMetadata)
    {
        // Map controller names to entity names
        var entityMapping = new Dictionary<string, string>
        {
            { "InteractionController", "Interaction" },
            { "PartnerController", "Partner" },
            { "ContactController", "Contact" }
        };
        
        if (entityMapping.TryGetValue(controllerInfo.Name, out var entityName))
        {
            controllerInfo.SearchMetadata = searchMetadata.FirstOrDefault(sm => sm.Entity == entityName);
        }
    }
}

// Extension method for nullable checking
public static class TypeExtensions
{
    public static bool IsNullable(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null ||
               !type.IsValueType ||
               (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }
} 