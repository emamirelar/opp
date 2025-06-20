using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.UNOPSBusiness.Interceptors;
using UNOPS.PAO.RBAC.Attributes;
using System.Reflection;

namespace UNOPS.PAO.UNOPSBusiness.Extensions;

public static class RBACServiceCollectionExtensions
{
    /// <summary>
    /// Registers a service with RBAC interceptor for automatic security enforcement
    /// </summary>
    public static IServiceCollection AddRBACService<TInterface, TImplementation>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
        
        services.Add(new ServiceDescriptor(typeof(TInterface), provider =>
        {
            var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
            var interceptor = provider.GetRequiredService<RBACInterceptor>();
            var implementation = provider.GetRequiredService<TImplementation>();
            
            return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        }, lifetime));

        return services;
    }

    /// <summary>
    /// Registers RBAC infrastructure services
    /// </summary>
    public static IServiceCollection AddRBACInfrastructure(this IServiceCollection services)
    {
        // Register Castle proxy generator
        services.AddSingleton<ProxyGenerator>();
        
        // Register RBAC interceptor
        services.AddScoped<RBACInterceptor>();
        
        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all managers with RBAC attributes
    /// Prioritizes UNOPS implementations over basic implementations
    /// </summary>
    public static IServiceCollection AddRBACManagersAutomatically(this IServiceCollection services, params Assembly[] assemblies)
    {
        // If no assemblies specified, scan the current assembly and common business assemblies
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[]
            {
                Assembly.GetExecutingAssembly(), // UNOPS.PAO.UNOPSBusiness
                Assembly.GetAssembly(typeof(UNOPS.PAO.Business.Interfaces.IPartnerManager)), // UNOPS.PAO.Business
            }.Where(a => a != null).ToArray();
        }

        var discoveredManagers = new Dictionary<Type, List<Type>>();

        // First pass: discover all manager implementations
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                
                foreach (var implementationType in types)
                {
                    // Skip abstract classes, interfaces, and generic type definitions
                    if (implementationType.IsAbstract || implementationType.IsInterface || implementationType.IsGenericTypeDefinition)
                        continue;

                    // Check if the type has any methods with RBAC attributes OR implements manager interfaces
                    var hasRBACMethods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Any(method => method.GetCustomAttribute<RBACAttribute>() != null || 
                                      method.GetCustomAttribute<SkipRBACAttribute>() != null);

                    // Also include types that implement manager interfaces even without RBAC attributes
                    var implementsManagerInterface = implementationType.GetInterfaces()
                        .Any(i => i.Name.EndsWith("Manager") && i.Namespace?.StartsWith("UNOPS.PAO") == true);

                    if (!hasRBACMethods && !implementsManagerInterface)
                        continue;

                    // Find interfaces that this type implements (excluding system interfaces)
                    var interfaces = implementationType.GetInterfaces()
                        .Where(i => !i.IsGenericType && 
                                   i.Namespace != null && 
                                   i.Namespace.StartsWith("UNOPS.PAO") &&
                                   i.Name.EndsWith("Manager"))
                        .ToList();

                    foreach (var interfaceType in interfaces)
                    {
                        if (!discoveredManagers.ContainsKey(interfaceType))
                        {
                            discoveredManagers[interfaceType] = new List<Type>();
                        }
                        discoveredManagers[interfaceType].Add(implementationType);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with other assemblies
                Console.WriteLine($"[RBAC] Warning: Could not scan assembly {assembly.FullName}: {ex.Message}");
            }
        }

        // Second pass: choose best implementation for each interface
        var managersToRegister = new List<(Type interfaceType, Type implementationType)>();
        
        foreach (var (interfaceType, implementations) in discoveredManagers)
        {
            Type bestImplementation;
            
            if (implementations.Count == 1)
            {
                bestImplementation = implementations[0];
            }
            else
            {
                // Multiple implementations found - prefer UNOPS implementations
                var unopsImplementation = implementations.FirstOrDefault(t => t.Name.StartsWith("UNOPS"));
                var rbacImplementation = implementations.FirstOrDefault(t => 
                    t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Any(method => method.GetCustomAttribute<RBACAttribute>() != null));
                
                // Priority order: UNOPS + RBAC > UNOPS > RBAC > first found
                bestImplementation = unopsImplementation ?? rbacImplementation ?? implementations[0];
                
                Console.WriteLine($"[RBAC] Multiple implementations found for {interfaceType.Name}:");
                foreach (var impl in implementations)
                {
                    var isChosen = impl == bestImplementation ? " (CHOSEN)" : "";
                    Console.WriteLine($"[RBAC]   - {impl.Name}{isChosen}");
                }
            }
            
            managersToRegister.Add((interfaceType, bestImplementation));
            Console.WriteLine($"[RBAC] Auto-discovered: {interfaceType.Name} -> {bestImplementation.Name}");
        }

        // Register all chosen managers
        foreach (var (interfaceType, implementationType) in managersToRegister)
        {
            try
            {
                var method = typeof(RBACServiceCollectionExtensions)
                    .GetMethod(nameof(AddRBACService))
                    ?.MakeGenericMethod(interfaceType, implementationType);
                
                method?.Invoke(null, new object[] { services, ServiceLifetime.Scoped });
                
                Console.WriteLine($"[RBAC] Registered: {interfaceType.Name} with RBAC interceptor");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RBAC] Error registering {interfaceType.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"[RBAC] Auto-registration complete. Registered {managersToRegister.Count} managers with RBAC interceptors.");
        
        return services;
    }

    /// <summary>
    /// Convenience method to register multiple RBAC services at once
    /// </summary>
    public static IServiceCollection AddRBACServices(this IServiceCollection services, Action<RBACServiceBuilder> configure)
    {
        var builder = new RBACServiceBuilder(services);
        configure(builder);
        return services;
    }
}

public class RBACServiceBuilder
{
    private readonly IServiceCollection _services;

    public RBACServiceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public RBACServiceBuilder AddService<TInterface, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        _services.AddRBACService<TInterface, TImplementation>(lifetime);
        return this;
    }
} 