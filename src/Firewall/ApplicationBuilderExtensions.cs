using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;

namespace Firewall
{
    /// <summary>
    /// Extension methods for registering Firewall middleware.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="FirewallMiddleware"/> to the ASP.NET Core pipeline.
        /// <para>The Firewall should be registered after global error handling and before any other middleware.</para>
        /// </summary>
        /// <param name="builder">The application builder which is used to register all ASP.NET Core middleware.</param>
        /// <param name="allowLocalRequests">Set to true if you want to allow requests from the local IP address (self).</param>
        /// <param name="vipList">A list of individual IP addresses which are allowed to make requests to the web application.</param>
        /// <param name="guestList">A list of IP address ranges which are allowed to make requests to the web application.</param>
        public static IApplicationBuilder UseFirewall(
            this IApplicationBuilder builder,
            bool allowLocalRequests = false,
            IList<IPAddress> vipList = null,
            IList<CIDRNotation> guestList = null) =>
                builder.UseMiddleware<FirewallMiddleware>(
                    allowLocalRequests,
                    vipList ?? new List<IPAddress>(),
                    guestList ?? new List<CIDRNotation>());

        /// <summary>
        /// Adds the <see cref="FirewallMiddleware"/> to the ASP.NET Core pipeline.
        /// <para>The Firewall should be registered after global error handling and before any other middleware.</para>
        /// </summary>
        /// <param name="builder">The application builder which is used to register all ASP.NET Core middleware.</param>
        /// <param name="allowLocalRequests">Set to true if you want to allow requests from the local IP address (self).</param>
        /// <param name="ipv4ListUrl">URL which returns a list of all Cloudflare IPv4 address ranges.</param>
        /// <param name="ipv6ListUrl">URL which returns a list of all Cloudflare IPv6 address ranges.</param>
        /// <param name="additionalVipList">A list of individual IP addresses which are allowed to make requests to the web application.</param>
        /// <param name="additionalGuestList">A list of IP address ranges which are allowed to make requests to the web application.</param>
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