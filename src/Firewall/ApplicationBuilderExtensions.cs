using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firewall
{
    ///
    public static class ServiceCollectionExtensions
    {
        ///
        public static IServiceCollection AddFirewall(this IServiceCollection services)
        {
            services.TryAddTransient<ICloudflareHelper, CloudflareHelper>();


            return services;
        }
    }

    /// <summary>
    /// Useful extension methods for registering Firewall middleware.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="FirewallMiddleware"/> to the ASP.NET Core pipeline.
        /// <para>The Firewall should be registered after the global error handling middleware and before any other middleware.</para>
        /// </summary>
        public static IApplicationBuilder UseFirewall(
            this IApplicationBuilder builder,
            bool allowLocalRequests = false,
            IList<IPAddress> vipList = null,
            IList<CIDRNotation> guestList = null)
        {
            return builder.UseMiddleware<FirewallMiddleware>(
                allowLocalRequests,
                vipList ?? new List<IPAddress>(),
                guestList ?? new List<CIDRNotation>());
        }

        /// <summary>
        /// Adds the <see cref="FirewallMiddleware"/> to the ASP.NET Core pipeline.
        /// <para>The Firewall should be registered after the global error handling middleware and before any other middleware.</para>
        /// </summary>
        public static IApplicationBuilder UseCloudflareFirewall(
            this IApplicationBuilder builder,
            bool allowLocalRequests = false,
            string ipv4ListUrl = null,
            string ipv6ListUrl = null,
            IList<IPAddress> additionalVipList = null,
            IList<CIDRNotation> additionalGuestList = null)
        {
            var helper = new CloudflareHelper(new HttpClient());
            var (vips, guests) = helper.GetIPAddressRanges(ipv4ListUrl, ipv6ListUrl);

            var vipList = vips.Concat(additionalVipList ?? new List<IPAddress>()).ToList();
            var guestList = guests.Concat(additionalGuestList ?? new List<CIDRNotation>()).ToList();

            return builder.UseMiddleware<FirewallMiddleware>(
                allowLocalRequests,
                vipList,
                guestList);
        }
    }
}