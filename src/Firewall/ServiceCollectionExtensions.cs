using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firewall
{
    /// <summary>
    /// Helper methods to register <see cref="Firewall"/> dependencies.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds <see cref="Firewall"/> dependencies to the <see cref="IServiceCollection"/> container.
        /// </summary>
        public static IServiceCollection AddFirewall(this IServiceCollection services)
        {
            services.TryAddTransient<ICloudflareHelper, CloudflareHelper>();
            return services;
        }
    }
}