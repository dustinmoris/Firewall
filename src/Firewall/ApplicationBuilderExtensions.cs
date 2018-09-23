using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;

namespace Firewall
{
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
                allowLocalRequests, vipList, guestList);
        }

        /// <summary>
        /// Adds the <see cref="CloudflareFirewallMiddleware"/> to the ASP.NET Core pipeline.
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
            return builder.UseMiddleware<CloudflareFirewallMiddleware>(
                allowLocalRequests,
                ipv4ListUrl,
                ipv6ListUrl,
                additionalVipList,
                additionalGuestList);
        }
    }
}