using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UNOPS.PAO.UNOPSIdentity.Authentication
{
    public static class IAPAuthenticationExtensions
    {
        /// <summary>
        /// Adds IAP verification middleware to validate Google IAP JWT tokens and headers
        /// </summary>
        public static IApplicationBuilder UseIAPVerification(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IAPVerificationMiddleware>();
        }
        
        /// <summary>
        /// Adds required services for IAP verification (HTTP client factory)
        /// </summary>
        public static IServiceCollection AddIAPVerification(this IServiceCollection services)
        {
            // Add HTTP client factory which is required for key retrieval
            services.AddHttpClient();
            
            return services;
        }
    }
} 